import { TestBed, async } from "@angular/core/testing";
import { AppComponent } from "./app.component";
import { PlayerComponent } from "./player/player.component";
import { RouterTestingModule } from "@angular/router/testing";
import { NavigationComponent } from "./navigation/navigation.component";

describe("AppComponent", () => {
  beforeEach(async(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      declarations: [AppComponent, PlayerComponent, NavigationComponent],
    }).compileComponents();
  }));

  it("should create the app", () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.debugElement.componentInstance;
    expect(app).toBeTruthy();
  });

  it(`should have as title 'LalaMachine'`, () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.debugElement.componentInstance;
    expect(app.title).toEqual("LalaMachine");
  });
});
