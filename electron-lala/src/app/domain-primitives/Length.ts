import { ValueIsUndefinedError } from "./ValueIsUndefinedError";
import { NoUndefinedValue } from "./NoUndefinedValue";

export class Length extends NoUndefinedValue<number> {
  constructor(seconds: number) {
    seconds = Math.round(seconds);
    super(seconds);

    if (seconds < 0 || seconds > 36000) {
      throw new RangeError();
    }

    this._value = seconds;
  }

  get seconds(): number {
    return this._value;
  }

  get asString(): string {
    let hours = Math.floor(this._value / 3600);
    let rem = this._value % 3600;

    let minutes = Math.floor(rem / 60);
    rem = rem % 60;

    let seconds = this.addLeadingZero(rem);

    if (hours > 0) {
      return `${hours}:${this.addLeadingZero(minutes)}:${seconds}`;
    }

    return `${minutes}:${seconds}`;
  }

  addLeadingZero(value: number): string {
    if (value < 10) {
      return `0${value}`;
    }

    return `${value}`;
  }
}
