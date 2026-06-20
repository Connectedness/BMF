// Live palette: each chip is a colour picker bound to a global CSS variable.
// Changing it repaints the whole page — logos, swatches, nav — in real time,
// because everything is themed off --graphite / --teal / --paper.

const root = document.documentElement;

for (const chip of document.querySelectorAll('.chip[data-var]')) {
  const input = chip.querySelector('input[type="color"]');
  const hex = chip.querySelector('.chip-hex');
  const apply = () => {
    root.style.setProperty(chip.dataset.var, input.value);
    if (hex) hex.textContent = input.value.toUpperCase();
  };
  input.addEventListener('input', apply);
  apply(); // sync on load
}
