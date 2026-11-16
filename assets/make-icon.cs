// Renders lalamachine.svg into the app's icons, at every size each one needs, with libvips:
// - assets/lalamachine.icns for macOS, build-macos.sh puts it into the app.
// - Lalamachine.Desktop/Assets/lalamachine.ico for Windows: the .exe, the windows, the setup and the Start menu.
//   Windows icons don't have the margin and shadow of the macOS grid, so it's only the rounded square.
// Usage, on a Mac: dotnet run assets/make-icon.cs
// Rerun this after editing the SVG.

#:package NetVips@3.2.0
#:package NetVips.Native.osx-x64@8.18.7
#:package NetVips.Native.osx-arm64@8.18.7

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NetVips;

if (!OperatingSystem.IsMacOS())
{
    Console.Error.WriteLine("Making the .icns needs a Mac (it uses iconutil).");
    return 1;
}

var assets = Path.GetDirectoryName(ThisFile())!;
var svg = File.ReadAllText(Path.Combine(assets, "lalamachine.svg"));
var icns = Path.Combine(assets, "lalamachine.icns");
var ico = Path.Combine(assets, "..", "Lalamachine.Desktop", "Assets", "lalamachine.ico");

WriteIcns(svg, icns);
Console.WriteLine($"Icon: {Path.GetFullPath(icns)}");

WriteIco(WindowsVersion(svg), ico);
Console.WriteLine($"Icon: {Path.GetFullPath(ico)}");
return 0;

// The rounded square alone: without the shadow, and the canvas cut down to the square.
static string WindowsVersion(string svg)
{
    svg = ReplaceOnce(svg, @"<rect id=""shadow""[^>]*/>", "");
    return ReplaceOnce(svg, @"width=""1024"" height=""1024"" viewBox=""0 0 1024 1024""", @"width=""824"" height=""824"" viewBox=""100 100 824 824""");
}

static string ReplaceOnce(string text, string pattern, string replacement)
{
    var matches = Regex.Count(text, pattern);
    if (matches != 1)
    {
        throw new InvalidOperationException($"Expected one match for {pattern} in the SVG, found {matches}.");
    }

    return Regex.Replace(text, pattern, replacement);
}

static Image Render(string svg, int pixels)
{
    var size = int.Parse(Regex.Match(svg, @"<svg[^>]* width=""(\d+)""").Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    var image = Image.SvgloadBuffer(System.Text.Encoding.UTF8.GetBytes(svg), scale: (double)pixels / size);
    if (image.Width != pixels || image.Height != pixels || image.Bands != 4)
    {
        throw new InvalidOperationException($"Expected {pixels} × {pixels} RGBA, got {image.Width} × {image.Height} with {image.Bands} bands; is the SVG still 1024 × 1024?");
    }

    return image;
}

static void WriteIcns(string svg, string icns)
{
    var iconset = Path.Combine(Path.GetTempPath(), $"lalamachine-{Guid.NewGuid():N}.iconset");
    Directory.CreateDirectory(iconset);
    try
    {
        // Point sizes, each at 1× and 2×: the names iconutil expects.
        foreach (var points in new[] { 16, 32, 128, 256, 512 })
        {
            foreach (var scale in new[] { 1, 2 })
            {
                using var image = Render(svg, points * scale);
                var name = scale == 1 ? $"icon_{points}x{points}.png" : $"icon_{points}x{points}@2x.png";
                image.WriteToFile(Path.Combine(iconset, name));
            }
        }

        using var iconutil = Process.Start("iconutil", ["--convert", "icns", "--output", icns, iconset])!;
        iconutil.WaitForExit();
        if (iconutil.ExitCode != 0)
        {
            throw new InvalidOperationException($"iconutil failed with exit code {iconutil.ExitCode}.");
        }
    }
    finally
    {
        Directory.Delete(iconset, recursive: true);
    }
}

// The sizes Windows picks from at 100 % to 250 % scaling. Like Windows' own icons, the small ones are bitmaps and
// 256 is a PNG: older readers (resource editors, installer builders) don't all understand PNGs at the small sizes.
static void WriteIco(string svg, string ico)
{
    int[] sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];
    var images = sizes.Select(size =>
    {
        using var image = Render(svg, size);
        return size == 256 ? image.PngsaveBuffer() : Dib(image);
    }).ToList();

    using var file = File.Create(ico);
    using var writer = new BinaryWriter(file);
    writer.Write((ushort)0); // reserved
    writer.Write((ushort)1); // an icon, not a cursor
    writer.Write((ushort)sizes.Length);
    var offset = 6 + 16 * sizes.Length;
    for (var i = 0; i < sizes.Length; i++)
    {
        writer.Write((byte)(sizes[i] % 256)); // 0 means 256
        writer.Write((byte)(sizes[i] % 256));
        writer.Write((byte)0); // no palette
        writer.Write((byte)0); // reserved
        writer.Write((ushort)1); // planes
        writer.Write((ushort)32); // bits per pixel
        writer.Write(images[i].Length);
        writer.Write(offset);
        offset += images[i].Length;
    }

    foreach (var image in images)
    {
        writer.Write(image);
    }
}

// An icon's bitmap: a BITMAPINFOHEADER with the height doubled, the BGRA rows bottom up, then the 1-bit mask (1 for
// transparent) that readers without alpha use.
static byte[] Dib(Image image)
{
    var size = image.Width;
    var rgba = image.WriteToMemory<byte>();
    var maskStride = (size + 31) / 32 * 4;
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    writer.Write(40); // header size
    writer.Write(size);
    writer.Write(size * 2); // the colours and the mask
    writer.Write((ushort)1); // planes
    writer.Write((ushort)32); // bits per pixel
    writer.Write(0); // BI_RGB
    writer.Write(size * size * 4 + maskStride * size);
    writer.Write(0); // horizontal resolution
    writer.Write(0); // vertical resolution
    writer.Write(0); // colours used
    writer.Write(0); // important colours

    for (var y = size - 1; y >= 0; y--)
    {
        for (var x = 0; x < size; x++)
        {
            var i = (y * size + x) * 4;
            writer.Write([rgba[i + 2], rgba[i + 1], rgba[i], rgba[i + 3]]);
        }
    }

    for (var y = size - 1; y >= 0; y--)
    {
        var row = new byte[maskStride];
        for (var x = 0; x < size; x++)
        {
            if (rgba[(y * size + x) * 4 + 3] == 0)
            {
                row[x / 8] |= (byte)(0x80 >> (x % 8));
            }
        }

        writer.Write(row);
    }

    writer.Flush();
    return stream.ToArray();
}

static string ThisFile([CallerFilePath] string path = "") => path;
