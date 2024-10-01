import { async, ComponentFixture, TestBed } from "@angular/core/testing";

import { MainPlaylistComponent } from "./main-playlist.component";

describe("MainPlaylistComponent", () => {
  let component: MainPlaylistComponent;
  let fixture: ComponentFixture<MainPlaylistComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [MainPlaylistComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainPlaylistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it("should create", () => {
    expect(component).toBeTruthy();
  });
});
