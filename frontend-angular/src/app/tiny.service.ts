import { Injectable } from '@angular/core';

const API_BASE = 'https://localhost:44356'; // change if backend uses different port

@Injectable()
export class TinyUrlService {
  async add(url: string, isPrivate: boolean){
    const res = await fetch(API_BASE + '/api/add', {
      method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify({ url, isPrivate })
    });
    return await res.json();
  }

  async getPublic(){
    const res = await fetch(API_BASE + '/api/public');
    return await res.json();
  }

  async delete(code: string){
    await fetch(API_BASE + '/api/delete/' + encodeURIComponent(code), { method: 'DELETE' });
  }
}
