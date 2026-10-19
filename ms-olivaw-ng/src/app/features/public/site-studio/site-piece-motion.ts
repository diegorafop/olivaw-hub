/** Visual feedback for a user action; no upload or background work. */
export function animateSitePiece(
  viewport: HTMLElement,
  spreadsheet: boolean,
  done: () => void,
): () => void {
  const doc = viewport.ownerDocument;
  const reduced = doc.defaultView?.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const source = doc.activeElement?.getBoundingClientRect();
  if (reduced || !source) {
    done();
    return () => {};
  }
  const destination = viewport.getBoundingClientRect();
  const piece = doc.createElement('div');
  piece.setAttribute('aria-hidden', 'true');
  const title = doc.createElement('strong');
  title.textContent = spreadsheet ? '▦  Minha planilha' : '✳  Sua ideia';
  Object.assign(title.style, {
    display: 'block',
    padding: '14px',
    background: '#256347',
    color: 'white',
    fontSize: '14px',
  });
  piece.append(title);
  if (spreadsheet) {
    const table = doc.createElement('table');
    Object.assign(table.style, {
      width: '100%',
      borderCollapse: 'collapse',
      fontSize: '11px',
      color: '#244536',
    });
    for (const values of [
      ['', 'A', 'B', 'C'],
      ['1', 'Produto', 'Estoque', 'Status'],
      ['2', 'Luminária', '12', 'OK'],
      ['3', 'Cadeira', '3', 'Repor'],
      ['4', 'Mesa', '8', 'OK'],
    ]) {
      const row = table.insertRow();
      for (const value of values) {
        const cell = row.insertCell();
        cell.textContent = value;
        Object.assign(cell.style, {
          padding: '9px 7px',
          border: '1px solid #d5e2d8',
          background: values[0] === '1' ? '#e4efe7' : '#fff',
        });
      }
    }
    piece.append(table);
  }
  Object.assign(piece.style, {
    position: 'fixed',
    left: source.left + 'px',
    top: source.top + 'px',
    width: spreadsheet ? '260px' : '130px',
    background: '#fff',
    borderRadius: '12px',
    overflow: 'hidden',
    zIndex: '1000',
    pointerEvents: 'none',
    transformOrigin: 'top left',
    boxShadow: '0 24px 64px #162c3940',
    border: '1px solid #bdd3c3',
  });
  doc.body.append(piece);
  const menu = viewport.querySelector<HTMLElement>('.site-tabs')?.getBoundingClientRect();
  const landingLeft = (menu?.left ?? destination.left) + Math.min(100, destination.width / 4);
  const landingTop =
    menu && menu.top >= destination.top && menu.top < destination.bottom
      ? menu.top + 6
      : destination.top + 65;
  const startX = Math.min(source.left, (doc.defaultView?.innerWidth ?? 1200) - 270);
  piece.style.left = startX + 'px';
  const x = landingLeft - startX;
  const y = landingTop - source.top;
  // A curved throw: lift, travel, decelerate, then fold into the navigation slot.
  const lift = Math.min(220, Math.max(70, source.top - 24));
  const frames: Keyframe[] = [];
  for (let step = 0; step <= 24; step++) {
    const t = step / 24;
    const progress = t * t * (3 - 2 * t);
    const travelX = x * progress;
    const travelY = y * progress - Math.sin(Math.PI * t) * lift;
    const spin = -12 * Math.sin(Math.PI * t) + 8 * Math.sin(2 * Math.PI * t);
    const tilt = 24 * Math.sin(Math.PI * t);
    const fold = Math.max(0, (t - 0.78) / 0.22);
    const scaleX = 0.75 + 0.25 * Math.sin(Math.PI * t) - 0.12 * fold;
    const scaleY = 0.75 + 0.25 * Math.sin(Math.PI * t) - 0.6 * fold;
    frames.push({
      offset: t,
      transform: `translate3d(${travelX}px,${travelY}px,0) perspective(700px) rotateY(${tilt}deg) rotateZ(${spin}deg) scale(${scaleX},${scaleY})`,
      opacity: t < 0.06 ? t / 0.06 : t > 0.96 ? (1 - t) / 0.04 : 1,
      borderRadius: `${12 + 20 * fold}px`,
      boxShadow: `0 ${16 + Math.sin(Math.PI * t) * 26}px ${32 + Math.sin(Math.PI * t) * 40}px #162c3940`,
    });
  }
  const animation = piece.animate(frames, {
    duration: spreadsheet ? 2600 : 1400,
    easing: 'linear',
    fill: 'forwards',
  });
  animation.onfinish = () => {
    piece.remove();
    done();
  };
  animation.oncancel = () => piece.remove();
  return () => {
    animation.cancel();
    piece.remove();
  };
}
