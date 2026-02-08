import type { Language } from './models';

export enum UITheme {
  Dark = 'Dark',
  Light = 'Light',
  Custom = 'Custom'
}

export interface UserEditorPreferenceDto {
  enableFlameEffects: boolean;
}

export interface UserCodePreferenceDto {
  defaultLanguage: Language;
}

export interface UserUIPreferenceDto {
  uiTheme: UITheme;
  pageSize: number;
}

export interface PreferencesDto {
  uiPreference: UserUIPreferenceDto;
  codePreference: UserCodePreferenceDto;
  editorPreference: UserEditorPreferenceDto;
}

export interface UserPreferencesDto {
  id: number;
  userId: number;
  lastUpdated: string;
  preferences: PreferencesDto;
}
