import { Card, CardBody, CardHeader } from '@heroui/react';

export function PreferencesSection() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-foreground mb-2">Preferences</h1>
        <p className="text-default-500">Customize your DerpCode experience</p>
      </div>

      <Card>
        <CardHeader>
          <h2 className="text-lg font-semibold">User Preferences</h2>
        </CardHeader>
        <CardBody>
          <p className="text-default-500">Preferences settings will be available in a future update.</p>
        </CardBody>
      </Card>
    </div>
  );
}
