import { ValueIsUndefinedError } from "./ValueIsUndefinedError";

export class DiscNumber {
  private _value: number;

  constructor(value: number) {
    if (value === undefined) {
      throw new ValueIsUndefinedError();
    }

    if (value < 0 || value > 99) {
      throw new RangeError();
    }

    this._value = value;
  }

  get value(): number {
    return this._value;
  }
}
