export interface ApiErrorContext {
  url?: string;
  status?: number;
  body?: unknown;
}

export function logError(source: string, error: unknown, api?: ApiErrorContext): void {
  const timestamp = new Date().toISOString();
  const message = error instanceof Error ? error.message : String(error);
  const entry: Record<string, unknown> = { timestamp, source, message };
  if (api?.url    != null) entry['url']    = api.url;
  if (api?.status != null) entry['status'] = api.status;
  if (api?.body   != null) entry['body']   = api.body;
  console.error('[GrindAtlas]', entry);
}
