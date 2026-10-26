import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

export interface TimelineEntry {
  actor: string;
  action: string;
  timestamp: string;
}

@Component({
  selector: 'app-timeline',
  imports: [MatIconModule],
  templateUrl: './timeline.html',
  styleUrl: './timeline.scss',
})
export class Timeline {
  @Input() entries!: TimelineEntry[];
}
