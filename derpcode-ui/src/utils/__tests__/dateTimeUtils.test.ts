import { daysSinceDate, isWithinTimeLimit } from '../dateTimeUtils';
import { jest } from '@jest/globals';

describe('dateTimeUtils', () => {
  beforeEach(() => {
    jest.useFakeTimers();
    jest.setSystemTime(new Date('2025-01-01T12:00:00.000Z'));
  });

  afterEach(() => {
    jest.useRealTimers();
  });

  test('isWithinTimeLimit returns false for undefined', () => {
    expect(isWithinTimeLimit(undefined, 10)).toBe(false);
  });

  test('isWithinTimeLimit returns true when within limit', () => {
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000).toISOString();
    expect(isWithinTimeLimit(fiveMinutesAgo, 10)).toBe(true);
  });

  test('isWithinTimeLimit returns false when outside limit', () => {
    const fifteenMinutesAgo = new Date(Date.now() - 15 * 60 * 1000).toISOString();
    expect(isWithinTimeLimit(fifteenMinutesAgo, 10)).toBe(false);
  });

  test('daysSinceDate floors days', () => {
    const twoDaysAgo = new Date(Date.now() - 2.4 * 24 * 60 * 60 * 1000).toISOString();
    expect(daysSinceDate(twoDaysAgo)).toBe(2);
  });
});
