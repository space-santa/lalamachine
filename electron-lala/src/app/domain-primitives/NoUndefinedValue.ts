import { ValueIsUndefinedError } from "./ValueIsUndefinedError";

export class NoUndefinedValue<T> {
  protected _value!: T;

  constructor(value: T) {
    if (value === undefined) {
      throw new ValueIsUndefinedError();
    }
  }
}
