// Minimal Jest setup for the UI unit tests.

// Some environments don't provide crypto.randomUUID.
if (!globalThis.crypto) {
  (globalThis as unknown as { crypto: Partial<Crypto> }).crypto = {};
}

if (!globalThis.crypto.randomUUID) {
  (globalThis.crypto as unknown as { randomUUID: () => string }).randomUUID = () =>
    '00000000-0000-0000-0000-000000000000';
}
