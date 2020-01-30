import { Injectable } from "@angular/core";
import { MediaUrl } from "../domain-primitives/MediaUrl";
import { Volume } from "../domain-primitives/Volume";
import { Subject } from "rxjs";
import { Length } from "../domain-primitives/Length";
import { Progress } from "../domain-primitives/Progress";
import { Track } from "../domain-primitives/Track";

@Injectable({
  providedIn: "root"
})
export class PlayerService {
  audioElement: HTMLAudioElement;

  positionChanged = new Subject<Length>();
  durationChanged = new Subject<Length>();
  trackChanged = new Subject<Track>();
  isPausedChanged = new Subject<boolean>();

  constructor() {
    this.audioElement = new Audio();
    this.audioElement.ondurationchange = event => {
      this.durationChanged.next(new Length(this.audioElement.duration));
    };
    this.audioElement.ontimeupdate = event => {
      this.positionChanged.next(new Length(this.audioElement.currentTime));
    };
  }

  get progress(): Progress {
    const duration = new Length(this.audioElement.duration);
    if (duration.seconds < 1) {
      return new Progress(0);
    }
    return new Progress(
      this.audioElement.currentTime / this.audioElement.duration
    );
  }

  playpause() {
    if (this.audioElement.readyState < 2) {
      return;
    }

    if (this.audioElement.paused) {
      this.audioElement.play();
    } else {
      this.audioElement.pause();
    }

    this.isPausedChanged.next(this.audioElement.paused);
  }

  private load(url: string) {
    this.audioElement.src = `http://127.0.0.1:3000/${url}`;
    this.audioElement.load();
  }

  play(track: Track) {
    this.load(track.mediaUrl);
    this.trackChanged.next(track);
    this.audioElement.play();
    this.isPausedChanged.next(this.audioElement.paused);
  }

  next() {
    this.play(Track.fakeTrack());
  }

  volume(value: Volume) {
    this.audioElement.volume = value.value;
  }
}
