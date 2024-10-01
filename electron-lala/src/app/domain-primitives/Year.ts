import { ValueIsUndefinedError } from "./ValueIsUndefinedError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class Year extends NoUndefinedValue<number> {
  constructor(year: number) {
    super(year);

    if (year < 1600 || year > 3000) {
      throw new RangeError("Year is out of range.");
    }
    this._value = year;
  }

  get value(): number {
    return this._value;
  }
}
