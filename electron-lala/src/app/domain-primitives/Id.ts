import { ValueIsUndefinedError } from "./ValueIsUndefinedError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class Id extends NoUndefinedValue<number> {
  constructor(value: number) {
    super(value);

    if (value < 1) {
      throw new RangeError();
    }

    this._value = value;
  }

  get value(): number {
    return this._value;
  }
}
