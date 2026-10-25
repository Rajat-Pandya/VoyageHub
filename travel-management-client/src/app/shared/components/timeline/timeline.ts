import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';


export interface TimelineEntry {
  actor: string;
  action: string;
  timestamp: string;
}

@Component({
  selector: 'app-timeline',
  imports: [CommonModule, MatIconModule],
  templateUrl: './timeline.html',
  styleUrl: './timeline.scss',
})
export class Timeline {
  @Input() entries! : TimelineEntry[];
}
