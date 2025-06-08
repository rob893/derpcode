export function isWithinTimeLimit(dateString: string | undefined, limitInMinutes: number): boolean {
  if (!dateString) return false;
  const date = new Date(dateString);
  const now = new Date();
  const diffInMs = now.getTime() - date.getTime();
  const diffInMinutes = diffInMs / (1000 * 60);
  return diffInMinutes <= limitInMinutes;
}

export function daysSinceDate(dateString: string): number {
  const date = new Date(dateString);
  const now = new Date();
  const diffInMs = now.getTime() - date.getTime();
  const diffInDays = diffInMs / (1000 * 60 * 60 * 24);
  return Math.floor(diffInDays);
}
