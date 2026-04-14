import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TinyUrlService } from './tiny.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  providers: [TinyUrlService]
})
export class AppComponent {
  url = '';
  isPrivate = false;
  items: any[] = [];
  search = '';
  result: any = null;

  constructor(private svc: TinyUrlService) {
    this.load();
  }

  async shorten(){
    if(!this.url) return;
    this.result = await this.svc.add(this.url, this.isPrivate);
    this.url = ''; this.isPrivate=false;
    await this.load();
  }

  async load(){
    this.items = await this.svc.getPublic();
  }

  async delete(code: string){
    if(!confirm('Delete '+code+'?')) return;
    await this.svc.delete(code);
    await this.load();
  }

  copy(code:string){
    const full = window.location.origin + '/' + code;
    navigator.clipboard.writeText(full);
  }
}
