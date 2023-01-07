import { NoUndefinedValue } from "./NoUndefinedValue";

export class Progress extends NoUndefinedValue<number> {
  constructor(value: number) {
    if (value < 1 || value > 0) {
      value = value * 100;
    }
    value = Math.round(value);
    super(value);

    if (value < 0 || value > 100) {
      throw new RangeError("Progress is out of range.");
    }
    this._value = value;
  }

  get value(): number {
    return this._value;
  }
}
