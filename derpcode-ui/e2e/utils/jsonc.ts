export function parseJsonc(text: string): unknown {
  // Remove // comments
  let cleaned = text.replace(/(^|[^:\\])\/\/.*$/gm, '$1');

  // Remove /* */ comments
  cleaned = cleaned.replace(/\/\*[\s\S]*?\*\//g, '');

  return JSON.parse(cleaned);
}
