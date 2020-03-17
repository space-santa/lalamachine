import { ValueIsUndefinedError } from "./ValueIsUndefinedError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class TrackNumber extends NoUndefinedValue<number> {
  constructor(value: number) {
    super(value);

    if (value < 0 || value > 999) {
      throw new RangeError();
    }

    this._value = value;
  }

  get value(): number {
    return this._value;
  }
}
