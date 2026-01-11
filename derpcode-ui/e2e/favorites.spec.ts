import { test, expect } from '@playwright/test';
import { getLlmUiCredentials } from './utils/credentials';

test('can favorite/unfavorite from list and details', async ({ page }) => {
  const { username, password } = getLlmUiCredentials();
  test.skip(!username || !password, 'LLM UI credentials not found in DerpCode.API/appsettings.Local.json');

  // Login
  await page.goto('/#/login');
  await page.locator('input[placeholder="Enter your username or email"]').fill(username!);
  await page.locator('input[placeholder="Enter your password"]').fill(password!);
  await page.getByRole('button', { name: /^sign in$/i }).click();

  // Go to problems list
  await page.goto('/#/problems');
  await expect(page.getByRole('heading', { name: 'Problems' })).toBeVisible();

  // Toggle favorite from list (first available)
  const listToggle = page.locator('[data-testid^="favorite-toggle-"]').first();
  await expect(listToggle).toBeVisible();

  const beforeLabel = (await listToggle.getAttribute('aria-label')) ?? '';
  await listToggle.click();

  // Ensure we didn't navigate by clicking the toggle inside the card
  await expect(page).toHaveURL(/\/#\/problems$/);

  const afterLabel = (await listToggle.getAttribute('aria-label')) ?? '';
  expect(afterLabel).not.toBe(beforeLabel);

  // Open first problem details by clicking its title
  await page.locator('h3').first().click();
  await expect(page).toHaveURL(/\/#\/problems\/[0-9]+$/);

  // Details toggle reflects same state
  const detailsToggle = page.locator('[data-testid="favorite-toggle-details"]:visible');
  await expect(detailsToggle).toBeVisible();
  await expect(detailsToggle).toHaveAttribute('aria-label', afterLabel);

  // Toggle back on details (restores original state)
  await detailsToggle.click();
  await expect(detailsToggle).toHaveAttribute('aria-label', beforeLabel);
});
