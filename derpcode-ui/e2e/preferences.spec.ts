import { test, expect } from '@playwright/test';
import { getLlmUiCredentials } from './utils/credentials';

test('can edit and persist preferences', async ({ page }) => {
  const { username, password } = getLlmUiCredentials();
  test.skip(!username || !password, 'LLM UI credentials not found in DerpCode.API/appsettings.Local.json');

  // Login
  await page.goto('/#/login');
  await page.locator('input[placeholder="Enter your username or email"]').fill(username!);
  await page.locator('input[placeholder="Enter your password"]').fill(password!);
  await page.getByRole('button', { name: /^sign in$/i }).click();

  // Login redirects to `/` first, and the landing page then redirects to `/problems`.
  // Wait for that to settle before navigating elsewhere to avoid racey redirects.
  await page.waitForURL('**/#/problems', { timeout: 30000 });

  // Navigate to Preferences
  await page.goto('/#/account');
  await page.getByRole('button', { name: 'Preferences' }).click();

  // Change a few values
  const pageSizeSelectButton = page.getByRole('button', { name: /page size/i });
  await pageSizeSelectButton.click();
  await page.getByRole('option', { name: /^10$/ }).click();

  await page.locator('[data-testid="preference-flames"]').click();

  // Wait for autosave
  await expect(page.locator('[data-testid="preferences-save-status"]')).toHaveText(/saved|saving/i);
  await expect(page.locator('[data-testid="preferences-save-status"]')).toHaveText(/saved/i, { timeout: 10000 });

  // Navigate away and back and confirm page size is retained in UI.
  // (Avoid a full browser refresh here: auth uses an in-memory access token.)
  await page.goto('/#/problems');
  await page.goto('/#/account');
  await page.getByRole('button', { name: 'Preferences' }).click();

  await expect(page.getByRole('button', { name: /page size/i })).toContainText('10');
});
