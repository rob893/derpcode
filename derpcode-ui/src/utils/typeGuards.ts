import { Language } from '../types/models';

export function isLanguage(value: string): value is Language {
  return Object.values(Language).includes(value as Language);
}
