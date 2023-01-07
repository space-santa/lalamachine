import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { PlayerComponent } from "./player/player.component";
import { NavigationComponent } from "./navigation/navigation.component";
import { ProgressBarComponent } from "./player/progress-bar/progress-bar.component";
import { MainPlaylistComponent } from "./main-playlist/main-playlist.component";
import { LibraryComponent } from "./library/library.component";
import { SettingsComponent } from "./settings/settings.component";

@NgModule({
  declarations: [
    AppComponent,
    PlayerComponent,
    NavigationComponent,
    ProgressBarComponent,
    MainPlaylistComponent,
    LibraryComponent,
    SettingsComponent
  ],
  imports: [BrowserModule, AppRoutingModule],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
