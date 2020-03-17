import { Component, OnInit } from "@angular/core";
import { PlayerService } from "./player.service";
import { Track } from "../domain-primitives/Track";

@Component({
  selector: "app-player",
  templateUrl: "./player.component.html",
  styleUrls: ["./player.component.css"]
})
export class PlayerComponent implements OnInit {
  selectedFile = "http://127.0.0.1:3000/files/4";
  currentPosition: string;
  totalLength: string;
  progress = 0;
  track: Track;
  isPaused = true;

  constructor(private player: PlayerService) {}

  ngOnInit() {
    this.player.trackChanged.subscribe(value => {
      this.track = value;
    });
    this.player.durationChanged.subscribe(value => {
      this.totalLength = value.asString;
      this.progress = this.player.progress.value;
    });
    this.player.positionChanged.subscribe(value => {
      this.currentPosition = value.asString;
      this.progress = this.player.progress.value;
    });
    this.player.isPausedChanged.subscribe(value => {
      this.isPaused = value;
    });
  }

  onPlayPause() {
    this.player.playpause();
  }

  onNext() {
    this.player.next();
  }
}
