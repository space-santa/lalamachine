import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MainPlaylistComponent } from "./main-playlist/main-playlist.component";
import { LibraryComponent } from "./library/library.component";
import { SettingsComponent } from "./settings/settings.component";

const routes: Routes = [
  { path: "", component: MainPlaylistComponent },
  { path: "library", component: LibraryComponent },
  { path: "settings", component: SettingsComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
