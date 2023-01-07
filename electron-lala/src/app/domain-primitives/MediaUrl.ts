import { SuffixError } from "./SuffixError";
import { InsecureUrlError } from "./InsecureUrlError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class MediaUrl extends NoUndefinedValue<string> {
  constructor(value: string) {
    super(value);

    if (!value.startsWith("https://")) {
      // throw new InsecureUrlError();
      // I am pretty sure that I don't want to have the full URL in the track.
      // The server shouldn't have to change, so it is just the path.
      // Or it could even just be UUID.
      console.log(`Insecure URL ${value}`);
    }

    if (value.endsWith(".mp3") || value.endsWith(".m4a")) {
      this._value = value;
    } else {
      throw new SuffixError();
    }
  }

  get value(): string {
    return this._value;
  }
}
