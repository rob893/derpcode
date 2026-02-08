import { Card, CardBody, CardHeader, Button, Select, SelectItem, Switch, Spinner } from '@heroui/react';
import { ArrowDownTrayIcon } from '@heroicons/react/24/outline';
import { useEffect, useMemo, useRef, useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { usePWA } from '../hooks/usePWA';
import { queryKeys, usePatchUserPreferences, useUserPreferences } from '../hooks/api';
import { useQueryClient } from '@tanstack/react-query';
import { buildDefaultUserPreferences, saveUserPreferencesToLocalStorage } from '../utils/userPreferencesStorage';
import { Language, type JsonPatchDocument } from '../types/models';
import { UITheme, type UserPreferencesDto } from '../types/userPreferences';

export function PreferencesSection() {
  const { isInstallable, promptInstall } = usePWA();
  const { user, isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const userId = user?.id;
  const queryClient = useQueryClient();

  const { data: preferences, isLoading: isPreferencesLoading, error: preferencesError } = useUserPreferences(userId);
  const patchPreferencesMutation = usePatchUserPreferences(userId ?? 0);

  const [draft, setDraft] = useState<UserPreferencesDto | null>(null);
  const [hasLocalEdits, setHasLocalEdits] = useState(false);
  const [saveStatus, setSaveStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');
  const [saveErrorMessage, setSaveErrorMessage] = useState<string | null>(null);

  const pendingPatchRef = useRef<Record<string, unknown>>({});
  const debounceTimerRef = useRef<number | null>(null);

  useEffect(() => {
    if (!userId) return;

    if (draft == null) {
      setDraft(preferences ?? buildDefaultUserPreferences(userId));
      return;
    }

    // If the user hasn't started editing, keep draft in sync with server updates.
    if (!hasLocalEdits && preferences) {
      setDraft(preferences);
    }
  }, [userId, preferences, draft, hasLocalEdits]);

  const flushPendingPatch = async (): Promise<void> => {
    if (!userId || !draft) return;

    const entries = Object.entries(pendingPatchRef.current);
    if (entries.length === 0) return;

    // If we don't have a server-backed preferences id, we can't PATCH.
    if (draft.id <= 0) {
      setSaveStatus('error');
      setSaveErrorMessage('Preferences are not available on the server yet. Changes are saved locally only.');
      return;
    }

    const patchDocument: JsonPatchDocument = entries.map(([path, value]) => ({
      op: 'replace' as const,
      path,
      value
    }));

    setSaveStatus('saving');
    setSaveErrorMessage(null);

    try {
      const updated = await patchPreferencesMutation.mutateAsync({ preferencesId: draft.id, patchDocument });
      pendingPatchRef.current = {};

      setDraft(updated);
      setHasLocalEdits(false);
      setSaveStatus('saved');

      // Keep local storage and query cache coherent.
      saveUserPreferencesToLocalStorage(userId, updated);
      queryClient.setQueryData(queryKeys.userPreferences(userId), updated);
    } catch (error: any) {
      setSaveStatus('error');
      setSaveErrorMessage(error?.message ?? 'Failed to save preferences');
    }
  };

  const scheduleFlush = (): void => {
    if (debounceTimerRef.current) {
      window.clearTimeout(debounceTimerRef.current);
    }

    debounceTimerRef.current = window.setTimeout(() => {
      flushPendingPatch();
    }, 700);
  };

  const updateDraft = (updater: (current: UserPreferencesDto) => UserPreferencesDto): void => {
    if (!userId) return;

    setHasLocalEdits(true);
    setSaveStatus('idle');
    setSaveErrorMessage(null);

    setDraft(current => {
      const base = current ?? preferences ?? buildDefaultUserPreferences(userId);
      const next = updater(base);

      saveUserPreferencesToLocalStorage(userId, next);
      queryClient.setQueryData(queryKeys.userPreferences(userId), next);

      return next;
    });

    scheduleFlush();
  };

  const handleInstall = async (): Promise<void> => {
    try {
      await promptInstall();
    } catch (error) {
      console.error('Failed to install PWA:', error);
    }
  };

  const themeValue = draft?.preferences.uiPreference.uiTheme ?? UITheme.Dark;
  const pageSizeValue = (draft?.preferences.uiPreference.pageSize ?? 5).toString();
  const defaultLanguageValue = draft?.preferences.codePreference.defaultLanguage ?? Language.JavaScript;
  const flamesEnabled = draft?.preferences.editorPreference.enableFlameEffects ?? true;

  const saveStatusText = useMemo(() => {
    if (saveStatus === 'saving') return 'Saving…';
    if (saveStatus === 'saved') return 'Saved';
    if (saveStatus === 'error') return 'Error saving';
    return '';
  }, [saveStatus]);

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Preferences</h1>
        <p className="text-default-500">Customize your DerpCode experience</p>
      </div>

      {isInstallable && (
        <Card>
          <CardHeader>
            <h2 className="text-lg font-semibold">App Installation</h2>
          </CardHeader>
          <CardBody className="space-y-3">
            <p className="text-default-500">Install DerpCode directly on your device!</p>
            <Button
              color="primary"
              onPress={handleInstall}
              startContent={<ArrowDownTrayIcon className="h-4 w-4" />}
              className="w-fit"
              isDisabled={!isInstallable}
            >
              {isInstallable ? 'Install' : 'Install Not Available'}
            </Button>
          </CardBody>
        </Card>
      )}

      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">User Preferences</h2>
        </CardHeader>
        <CardBody className="space-y-6">
          {(isAuthLoading || !isAuthenticated) && (
            <div className="flex items-center gap-2 text-default-500">
              <Spinner size="sm" color="primary" />
              <span>Loading authentication…</span>
            </div>
          )}

          {userId && isPreferencesLoading && (
            <div className="flex items-center gap-2 text-default-500">
              <Spinner size="sm" color="primary" />
              <span>Loading preferences…</span>
            </div>
          )}

          {preferencesError && <div className="text-danger">Failed to load preferences</div>}

          {userId && draft && (
            <div className="space-y-5">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="space-y-2">
                  <div>
                    <h3 className="text-medium font-semibold">UI Theme</h3>
                    <p className="text-small text-default-500">Preferred app theme</p>
                  </div>
                  <Select
                    selectedKeys={[themeValue]}
                    onSelectionChange={keys => {
                      const selected = Array.from(keys)[0] as UITheme | undefined;
                      if (!selected) return;

                      pendingPatchRef.current['/preferences/uiPreference/uiTheme'] = selected;
                      updateDraft(current => ({
                        ...current,
                        preferences: {
                          ...current.preferences,
                          uiPreference: {
                            ...current.preferences.uiPreference,
                            uiTheme: selected
                          }
                        }
                      }));
                    }}
                    className="max-w-xs"
                    variant="bordered"
                    aria-label="UI theme"
                    name="preference-ui-theme"
                  >
                    <SelectItem key={UITheme.Dark}>Dark</SelectItem>
                    <SelectItem key={UITheme.Light}>Light</SelectItem>
                    <SelectItem key={UITheme.Custom}>Custom</SelectItem>
                  </Select>
                </div>

                <div className="space-y-2">
                  <div>
                    <h3 className="text-medium font-semibold">Page Size</h3>
                    <p className="text-small text-default-500">Default items per page</p>
                  </div>
                  <Select
                    selectedKeys={[pageSizeValue]}
                    onSelectionChange={keys => {
                      const selected = Array.from(keys)[0] as string | undefined;
                      const parsed = selected ? Number.parseInt(selected, 10) : Number.NaN;
                      if (!Number.isFinite(parsed)) return;

                      pendingPatchRef.current['/preferences/uiPreference/pageSize'] = parsed;
                      updateDraft(current => ({
                        ...current,
                        preferences: {
                          ...current.preferences,
                          uiPreference: {
                            ...current.preferences.uiPreference,
                            pageSize: parsed
                          }
                        }
                      }));
                    }}
                    className="max-w-xs"
                    variant="bordered"
                    aria-label="Page size"
                    name="preference-page-size"
                  >
                    <SelectItem key="5">5</SelectItem>
                    <SelectItem key="10">10</SelectItem>
                    <SelectItem key="25">25</SelectItem>
                    <SelectItem key="50">50</SelectItem>
                  </Select>
                </div>

                <div className="space-y-2">
                  <div>
                    <h3 className="text-medium font-semibold">Default Language</h3>
                    <p className="text-small text-default-500">Default language for new sessions</p>
                  </div>
                  <Select
                    selectedKeys={[defaultLanguageValue]}
                    onSelectionChange={keys => {
                      const selected = Array.from(keys)[0] as Language | undefined;
                      if (!selected) return;

                      pendingPatchRef.current['/preferences/codePreference/defaultLanguage'] = selected;
                      updateDraft(current => ({
                        ...current,
                        preferences: {
                          ...current.preferences,
                          codePreference: {
                            ...current.preferences.codePreference,
                            defaultLanguage: selected
                          }
                        }
                      }));
                    }}
                    className="max-w-xs"
                    variant="bordered"
                    aria-label="Default language"
                    name="preference-default-language"
                  >
                    <SelectItem key={Language.CSharp}>C#</SelectItem>
                    <SelectItem key={Language.JavaScript}>JavaScript</SelectItem>
                    <SelectItem key={Language.TypeScript}>TypeScript</SelectItem>
                    <SelectItem key={Language.Python}>Python</SelectItem>
                    <SelectItem key={Language.Java}>Java</SelectItem>
                    <SelectItem key={Language.Rust}>Rust</SelectItem>
                  </Select>
                </div>

                <div className="space-y-2">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-medium font-semibold">Flame Effects</h3>
                      <p className="text-small text-default-500">Show fire animations when typing</p>
                    </div>
                    <Switch
                      isSelected={flamesEnabled}
                      onValueChange={value => {
                        pendingPatchRef.current['/preferences/editorPreference/enableFlameEffects'] = value;
                        updateDraft(current => ({
                          ...current,
                          preferences: {
                            ...current.preferences,
                            editorPreference: {
                              ...current.preferences.editorPreference,
                              enableFlameEffects: value
                            }
                          }
                        }));
                      }}
                      color="primary"
                      aria-label="Toggle flame effects"
                      data-testid="preference-flames"
                    />
                  </div>
                </div>
              </div>

              <div className="flex items-center gap-3">
                {saveStatus === 'saving' && <Spinner size="sm" color="primary" />}
                <span
                  className={saveStatus === 'error' ? 'text-danger text-small' : 'text-default-500 text-small'}
                  data-testid="preferences-save-status"
                >
                  {saveStatusText}
                </span>
                {saveStatus === 'error' && saveErrorMessage && (
                  <span className="text-danger text-small">{saveErrorMessage}</span>
                )}
              </div>
            </div>
          )}
        </CardBody>
      </Card>
    </div>
  );
}
