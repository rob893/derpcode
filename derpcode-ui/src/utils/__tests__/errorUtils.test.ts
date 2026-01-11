import { getErrorMessage } from '../errorUtils';
import { ApiError } from '../../types/errors';

describe('errorUtils', () => {
  test('returns ApiError allErrors joined when present', () => {
    const problemDetails = {
      type: 'x',
      title: 'x',
      status: 400,
      detail: 'x',
      instance: 'x',
      extensions: {
        correlationId: 'c',
        errors: ['one', 'two'],
        traceId: 't'
      }
    };

    const err = new ApiError('base', 400, 'Bad Request', problemDetails);
    expect(getErrorMessage(err)).toBe('one, two');
  });

  test('returns Error.message for Error', () => {
    expect(getErrorMessage(new Error('boom'))).toBe('boom');
  });

  test('returns string as-is', () => {
    expect(getErrorMessage('nope')).toBe('nope');
  });

  test('returns object.message when present', () => {
    expect(getErrorMessage({ message: 'hi' })).toBe('hi');
  });

  test('falls back to default message', () => {
    expect(getErrorMessage(123, 'default')).toBe('default');
  });
});
