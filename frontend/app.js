const API_BASE = window.API_BASE || 'https://localhost:7001'; // adjust port if needed

const urlInput = document.getElementById('urlInput');
const privateCheckbox = document.getElementById('privateCheckbox');
const shortenBtn = document.getElementById('shortenBtn');
const shortenResult = document.getElementById('shortenResult');
const listEl = document.getElementById('list');
const searchInput = document.getElementById('search');
const refreshBtn = document.getElementById('refreshBtn');

async function shorten() {
  const url = urlInput.value.trim();
  const isPrivate = privateCheckbox.checked;
  shortenResult.textContent = '';
  if (!url) { shortenResult.textContent = 'Please enter a URL'; return; }

  try {
    const res = await fetch(API_BASE + '/api/add', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ url, isPrivate })
    });
    if (!res.ok) {
      const err = await res.json().catch(()=>null);
      shortenResult.textContent = 'Error: ' + (err?.error || res.statusText);
      return;
    }
    const data = await res.json();
    const short = `${window.location.origin}/${data.shortCode}`;
    shortenResult.innerHTML = `Short URL: <a href="${short}" target="_blank">${short}</a> <button id="copyBtn">Copy</button>`;
    document.getElementById('copyBtn').addEventListener('click', ()=>{navigator.clipboard.writeText(short)});
    urlInput.value=''; privateCheckbox.checked=false;
    loadList();
  } catch (e) {
    shortenResult.textContent = 'Network error';
  }
}

async function loadList() {
  listEl.innerHTML = '<li>Loading...</li>';
  try {
    const res = await fetch(API_BASE + '/api/public');
    const items = await res.json();
    renderList(items);
  } catch (e) {
    listEl.innerHTML = '<li>Error loading list</li>';
  }
}

function renderList(items) {
  const q = searchInput.value.trim().toLowerCase();
  const filtered = items.filter(i => !q || i.shortCode.toLowerCase().includes(q) || i.originalUrl.toLowerCase().includes(q));
  if (!filtered.length) { listEl.innerHTML = '<li>No items</li>'; return; }
  listEl.innerHTML = '';
  filtered.forEach(i => {
    const li = document.createElement('li');
    const left = document.createElement('div');
    left.innerHTML = `<span class="code">${i.shortCode}</span> <a href="${i.originalUrl}" target="_blank">${i.originalUrl}</a> <div>Clicks: ${i.hits}</div>`;
    const actions = document.createElement('div'); actions.className='actions';
    const copyBtn = document.createElement('button'); copyBtn.textContent='Copy';
    copyBtn.addEventListener('click', ()=>{navigator.clipboard.writeText(window.location.origin + '/' + i.shortCode)});
    const delBtn = document.createElement('button'); delBtn.textContent='Delete';
    delBtn.addEventListener('click', ()=> deleteCode(i.shortCode));
    actions.appendChild(copyBtn); actions.appendChild(delBtn);
    li.appendChild(left); li.appendChild(actions);
    listEl.appendChild(li);
  });
}

async function deleteCode(code) {
  if (!confirm('Delete ' + code + '?')) return;
  try {
    const res = await fetch(API_BASE + '/api/delete/' + encodeURIComponent(code), { method: 'DELETE' });
    if (res.ok) loadList(); else alert('Delete failed');
  } catch (e) { alert('Network error'); }
}

shortenBtn.addEventListener('click', shorten);
refreshBtn.addEventListener('click', loadList);
searchInput.addEventListener('input', ()=> loadList());

// initial
loadList();
