/// <reference types="node" />

import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { parseJsonc } from './jsonc';

type AppSettingsLocal = {
  LLMSecrets?: {
    UIUserName?: string;
    UIPassword?: string;
  };
};

export function getLlmUiCredentials(): { username?: string; password?: string } {
  const __filename = fileURLToPath(import.meta.url);
  const __dirname = path.dirname(__filename);

  const repoRoot = path.resolve(__dirname, '..', '..', '..');
  const appSettingsPath = path.join(repoRoot, 'DerpCode.API', 'appsettings.Local.json');

  if (!fs.existsSync(appSettingsPath)) {
    return {};
  }

  const raw = fs.readFileSync(appSettingsPath, 'utf8');
  const parsed = parseJsonc(raw) as AppSettingsLocal;

  return {
    username: parsed.LLMSecrets?.UIUserName,
    password: parsed.LLMSecrets?.UIPassword
  };
}
