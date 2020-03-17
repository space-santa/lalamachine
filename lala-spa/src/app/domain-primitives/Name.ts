import { ValueIsUndefinedError } from "./ValueIsUndefinedError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class Name extends NoUndefinedValue<string> {
  constructor(value: string) {
    super(value);

    value = value.trim();
    if (value.length < 1 || value.length > 99) {
      throw new RangeError();
    }

    this._value = value;
  }

  get value(): string {
    return this._value;
  }
}
