let ctx, master, cache = new Map();

export async function init() {
    if (ctx) return;
    ctx = new (window.AudioContext || window.webkitAudioContext)({ latencyHint: "interactive" });
    master = ctx.createGain(); master.connect(ctx.destination);
    master.gain.value = 1;
}

export async function unlock() {
    await init();
    if (ctx.state === "suspended") await ctx.resume();
    return ctx.state;
}

async function load(url) {
    if (cache.has(url)) return cache.get(url);
    const res = await fetch(url); const arr = await res.arrayBuffer();
    const buf = await new Promise((ok, err)=>ctx.decodeAudioData(arr, ok, err));
    cache.set(url, buf); return buf;
}

export async function preload(urls) {
    await init(); await Promise.all(urls.map(load));
}

export async function play(url, volume = 1.0) {
    await init();
    const buf = await load(url);
    const src = ctx.createBufferSource(); src.buffer = buf;
    const g = ctx.createGain(); g.gain.value = Math.max(0, Math.min(2, volume));
    src.connect(g).connect(master); src.start();
}