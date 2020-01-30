import { Id } from "./Id";
import { Name } from "./Name";

export class NameWithId {
  private _id: Id;
  private _name: Name;

  constructor(id: number, name: string) {
    this._id = new Id(id);
    this._name = new Name(name);
  }

  static fromJson(json: any): NameWithId {
    return new NameWithId(json["id"], json["name"]);
  }

  get id(): number {
    return this._id.value;
  }

  get name(): string {
    return this._name.value;
  }
}
