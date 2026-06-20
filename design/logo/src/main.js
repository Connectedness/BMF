// Live tuner for the Concept 1 monogram.
// Rebuilds the SVG geometry from two parameters — base thickness and diamond
// size — plus the chosen junction variant, and paints every preview at once.

const VIEWBOX = '0 0 96 80';
const CX = 48; // horizontal centre (midpoint of the two legs at x=18 / x=78)

const state = {
  strokeW: 8,
  diamond: 17, // centre-to-tip distance of the diamond
  variant: 'A',
};

// Build the inner SVG markup for the current parameters.
function buildInner({ strokeW, diamond: d, variant }) {
  const cy = variant === 'C' ? 38 : 44; // raised exchange for C
  const nodeR = strokeW; // node weight tracks line weight
  const L = d * Math.SQRT2; // square side that yields a diamond of half-diagonal d
  const rectX = (CX - L / 2).toFixed(2);
  const rectY = (cy - L / 2).toFixed(2);
  const rx = (L * 0.12).toFixed(2);

  let path;
  if (variant === 'A') {
    // diagonals meet at the diamond's top tip
    path = `M18 64 V16 L${CX} ${(cy - d).toFixed(2)} L78 16 V64`;
  } else {
    // diagonals dock into the middle of the two upper edges
    const ulx = (CX - d / 2).toFixed(2);
    const urx = (CX + d / 2).toFixed(2);
    const uy = (cy - d / 2).toFixed(2);
    path = `M18 64 V16 L${ulx} ${uy} M${urx} ${uy} L78 16 V64`;
  }

  return (
    `<path d="${path}" class="route" stroke-width="${strokeW}" stroke-linecap="round" stroke-linejoin="round"/>` +
    `<rect x="${rectX}" y="${rectY}" width="${L.toFixed(2)}" height="${L.toFixed(2)}" rx="${rx}" transform="rotate(45 ${CX} ${cy})" class="exchange"/>` +
    `<circle cx="18" cy="64" r="${nodeR}" class="node"/>` +
    `<circle cx="78" cy="64" r="${nodeR}" class="node"/>`
  );
}

const previews = [...document.querySelectorAll('.tune-svg')];

function render() {
  const inner = buildInner(state);
  for (const svg of previews) {
    svg.setAttribute('viewBox', VIEWBOX);
    svg.innerHTML = inner;
  }
  document.querySelector('#thickness-val').textContent = state.strokeW.toFixed(1);
  document.querySelector('#diamond-val').textContent = state.diamond.toFixed(1);
}

// --- wire up controls ---
const thickness = document.querySelector('#thickness');
const diamond = document.querySelector('#diamond');

thickness.addEventListener('input', (e) => {
  state.strokeW = parseFloat(e.target.value);
  render();
});
diamond.addEventListener('input', (e) => {
  state.diamond = parseFloat(e.target.value);
  render();
});

for (const btn of document.querySelectorAll('.variant-btn')) {
  btn.addEventListener('click', () => {
    state.variant = btn.dataset.variant;
    document.querySelectorAll('.variant-btn').forEach((b) =>
      b.classList.toggle('active', b === btn)
    );
    render();
  });
}

// sync initial control positions with state, then paint
thickness.value = state.strokeW;
diamond.value = state.diamond;
render();
