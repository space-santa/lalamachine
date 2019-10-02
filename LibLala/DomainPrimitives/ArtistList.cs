using System.Collections.Generic;

namespace LibLala.DomainPrimitives
{
    public class ArtistList : BaseNameStringList<ArtistName>
    {
        public ArtistList(List<string> artistStrings) : base(artistStrings) { }
        public ArtistList(string artistsCsvString) : base(artistsCsvString) { }
    }
}
