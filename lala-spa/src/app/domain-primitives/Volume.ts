import { NoUndefinedValue } from "./NoUndefinedValue";

export class Volume extends NoUndefinedValue<number> {
  constructor(value: number) {
    super(value);

    if (value < 0 || value > 1) {
      throw new RangeError("Volume is out of range.");
    }
    this._value = value;
  }

  get value(): number {
    return this._value;
  }
}
