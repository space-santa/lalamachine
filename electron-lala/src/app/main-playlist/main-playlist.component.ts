import { Component, OnInit } from "@angular/core";
import { Track } from "../domain-primitives/Track";
import { PlayerService } from "../player/player.service";

@Component({
  selector: "app-main-playlist",
  templateUrl: "./main-playlist.component.html",
  styleUrls: ["./main-playlist.component.css"],
})
export class MainPlaylistComponent implements OnInit {
  tracks: Track[] = [];
  currentIndex!: number;
  doubleClickInProgress = false;
  doubleClickTimeout: number | undefined;

  constructor(private player: PlayerService) {
    for (let i = 0; i < 55; ++i) {
      const track = Track.fakeTrack();
      this.tracks.push(track);
    }
  }

  ngOnInit() {}

  addTrack(track: Track) {
    this.tracks.push(track);
  }

  addTracks(tracks: Track[]) {
    this.tracks.push(...tracks);
  }

  selectTrack(position: number): Track {
    this.currentIndex = position;
    return this.currentTrack;
  }

  playTrack(position: number) {
    if (this.doubleClickInProgress) {
      clearTimeout(this.doubleClickTimeout);
      this.doubleClickInProgress = false;
      console.log(`playTrack(${position})`);
      this.player.play(this.selectTrack(position));
    } else {
      this.doubleClickInProgress = true;
      this.doubleClickTimeout = setTimeout(() => {
        console.log("cancel timeout");
        this.doubleClickInProgress = false;
      }, 300) as unknown as number;
    }
  }

  get currentTrack(): Track {
    return this.tracks[this.currentIndex];
  }

  get nextTrack(): Track {
    return this.tracks[++this.currentIndex];
  }

  get previousTrack(): Track {
    return this.tracks[--this.currentIndex];
  }
}
