import { Card, CardBody, CardHeader } from '@heroui/react';
import { AppHeader } from '../components/AppHeader';

export function PrivacyPolicyPage() {
  return (
    <div className="min-h-screen bg-background">
      <AppHeader />

      <div className="container mx-auto px-6 py-12 max-w-4xl">
        <Card className="shadow-xl">
          <CardHeader className="pb-6">
            <div className="w-full text-center">
              <h1 className="text-4xl font-bold text-primary mb-2">Privacy Policy</h1>
              <p className="text-default-600">Last updated: June 2, 2025</p>
            </div>
          </CardHeader>

          <CardBody className="prose prose-lg max-w-none">
            <div className="space-y-8 text-foreground">
              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Introduction</h2>
                <p className="text-foreground/80 leading-relaxed">
                  Welcome to DerpCode! This Privacy Policy explains how we collect, use, disclose, and safeguard your
                  information when you visit our coding practice platform. Please read this privacy policy carefully. If
                  you do not agree with the terms of this privacy policy, please do not access the site.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Information We Collect</h2>

                <h3 className="text-xl font-medium text-secondary mb-3">Personal Information</h3>
                <p className="text-foreground/80 leading-relaxed mb-4">
                  We may collect personally identifiable information, such as:
                </p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>Username and email address when you create an account</li>
                  <li>Profile information you choose to provide</li>
                  <li>OAuth information when you sign in with GitHub or Google</li>
                  <li>Communications with us, including support requests</li>
                </ul>

                <h3 className="text-xl font-medium text-secondary mb-3 mt-6">Usage Information</h3>
                <p className="text-foreground/80 leading-relaxed mb-4">
                  We automatically collect certain information when you use our platform:
                </p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>Code submissions and solutions you write</li>
                  <li>Problems you solve and your progress</li>
                  <li>Device information and browser type</li>
                  <li>IP address and location data</li>
                  <li>Usage patterns and interaction data</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">How We Use Your Information</h2>
                <p className="text-foreground/80 leading-relaxed mb-4">We use the information we collect to:</p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>Provide, operate, and maintain our platform</li>
                  <li>Improve and personalize your experience</li>
                  <li>Process your code submissions and provide feedback</li>
                  <li>Communicate with you about your account and our services</li>
                  <li>Monitor usage and detect technical issues</li>
                  <li>Prevent fraud and enhance security</li>
                  <li>Comply with legal obligations</li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Information Sharing and Disclosure</h2>
                <p className="text-foreground/80 leading-relaxed mb-4">
                  We do not sell, trade, or otherwise transfer your personal information to third parties except in the
                  following circumstances:
                </p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>
                    <strong>Service Providers:</strong> We may share information with trusted third-party service
                    providers who assist in operating our platform
                  </li>
                  <li>
                    <strong>Legal Requirements:</strong> We may disclose information when required by law or to protect
                    our rights, property, or safety
                  </li>
                  <li>
                    <strong>Business Transfers:</strong> Information may be transferred in connection with a merger,
                    acquisition, or sale of assets
                  </li>
                  <li>
                    <strong>With Your Consent:</strong> We may share information with your explicit consent
                  </li>
                </ul>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Data Security</h2>
                <p className="text-foreground/80 leading-relaxed">
                  We implement appropriate technical and organizational security measures to protect your personal
                  information against unauthorized access, alteration, disclosure, or destruction. These measures
                  include encryption, secure server hosting, and regular security assessments. However, no method of
                  transmission over the internet or electronic storage is 100% secure, and we cannot guarantee absolute
                  security.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Cookies and Tracking Technologies</h2>
                <p className="text-foreground/80 leading-relaxed mb-4">
                  We use cookies and similar tracking technologies to:
                </p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>Keep you logged in to your account</li>
                  <li>Remember your preferences and settings</li>
                  <li>Analyze how you use our platform</li>
                  <li>Provide security features</li>
                </ul>
                <p className="text-foreground/80 leading-relaxed mt-4">
                  You can control cookies through your browser settings, but disabling cookies may affect the
                  functionality of our platform.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Third-Party Services</h2>
                <p className="text-foreground/80 leading-relaxed">
                  Our platform may integrate with third-party services such as GitHub and Google for authentication.
                  These services have their own privacy policies, and we encourage you to review them. We are not
                  responsible for the privacy practices of these third-party services.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Data Retention</h2>
                <p className="text-foreground/80 leading-relaxed">
                  We retain your personal information for as long as necessary to provide our services, comply with
                  legal obligations, resolve disputes, and enforce our agreements. When you delete your account, we will
                  delete or anonymize your personal information, except where we are required to retain it by law.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Your Rights and Choices</h2>
                <p className="text-foreground/80 leading-relaxed mb-4">
                  Depending on your location, you may have certain rights regarding your personal information:
                </p>
                <ul className="list-disc pl-6 space-y-2 text-foreground/80">
                  <li>
                    <strong>Access:</strong> Request a copy of the personal information we hold about you
                  </li>
                  <li>
                    <strong>Correction:</strong> Request correction of inaccurate or incomplete information
                  </li>
                  <li>
                    <strong>Deletion:</strong> Request deletion of your personal information
                  </li>
                  <li>
                    <strong>Portability:</strong> Request transfer of your data to another service
                  </li>
                  <li>
                    <strong>Objection:</strong> Object to certain processing of your information
                  </li>
                </ul>
                <p className="text-foreground/80 leading-relaxed mt-4">
                  To exercise these rights, please contact us using the information provided below.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Children's Privacy</h2>
                <p className="text-foreground/80 leading-relaxed">
                  Our platform is not intended for children under the age of 13. We do not knowingly collect personal
                  information from children under 13. If you are a parent or guardian and believe your child has
                  provided us with personal information, please contact us so we can delete such information.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">International Data Transfers</h2>
                <p className="text-foreground/80 leading-relaxed">
                  Your information may be transferred to and processed in countries other than your own. These countries
                  may have different data protection laws. We will take appropriate measures to ensure your information
                  receives adequate protection in accordance with this privacy policy.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Changes to This Privacy Policy</h2>
                <p className="text-foreground/80 leading-relaxed">
                  We may update this privacy policy from time to time. We will notify you of any material changes by
                  posting the new privacy policy on this page and updating the "Last updated" date. Your continued use
                  of our platform after any changes constitutes acceptance of the updated privacy policy.
                </p>
              </section>

              <section>
                <h2 className="text-2xl font-semibold text-primary mb-4">Contact Us</h2>
                <p className="text-foreground/80 leading-relaxed">
                  If you have any questions about this Privacy Policy or our privacy practices, please contact us at:
                </p>
                <div className="mt-4 p-4 bg-content2 rounded-lg">
                  <p className="text-foreground/80">
                    <strong>Email:</strong> rwherber@gmail.com
                  </p>
                </div>
              </section>
            </div>
          </CardBody>
        </Card>
      </div>
    </div>
  );
}
