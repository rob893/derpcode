import { useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { useEffect } from 'react';
import { Button, Card, CardBody } from '@heroui/react';
import { AppHeader } from '../components/AppHeader';

export function LandingPage() {
  const { isAuthenticated } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (isAuthenticated) {
      // If user is authenticated, redirect to problems
      navigate('/problems', { replace: true });
    }
  }, [isAuthenticated, navigate]);

  return (
    <div className="min-h-screen bg-background flex flex-col">
      <AppHeader />

      <main className="flex-1 container mx-auto px-6 py-12">
        <section className="text-center mb-20">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-5xl font-bold mb-6 bg-linear-to-r from-primary via-secondary to-primary bg-clip-text text-transparent leading-tight pb-2">
              Practice Coding with DerpCode
            </h2>
            <p className="text-xl text-foreground/80 mb-8 leading-relaxed">
              Tired of working at Wendy's (or the dumpster behind it)? Tired of being poor because you lost all your
              money following advice from r/WallStreetBets? Sharpen your programming skills with{' '}
              <span className="font-bold mb-6 bg-linear-to-r from-primary via-secondary to-primary bg-clip-text text-transparent">
                DerpCode
              </span>{' '}
              and land that FAANG job you've always dreamed of!{' '}
              <span className="text-xs italic text-foreground/40 font-light align-top">
                (or at least a job that pays more than minimum wage)
              </span>
            </p>
            <div className="flex gap-4 justify-center">
              <Button onPress={() => navigate('/register')} color="primary" size="lg" variant="solid">
                Get Started
              </Button>
              <Button onPress={() => navigate('/login')} variant="ghost" color="primary" size="lg">
                Sign In
              </Button>
            </div>
          </div>
        </section>

        <section className="mb-12">
          <h3 className="text-3xl font-bold text-center mb-12 text-foreground">Why Choose DerpCode?</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">💡</div>
                <h4 className="text-xl font-semibold mb-3 text-primary">Multiple Languages</h4>
                <p className="text-foreground/70">
                  Practice in JavaScript, TypeScript, Python, and more. Because apparently knowing one language isn't
                  enough to get past HR these days.
                </p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">🎯</div>
                <h4 className="text-xl font-semibold mb-3 text-secondary">Difficulty Levels</h4>
                <p className="text-foreground/70">
                  From "hello world" easy to "why did I choose this career" hard. We'll crush your confidence
                  systematically!
                </p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">⚡</div>
                <h4 className="text-xl font-semibold mb-3 text-primary">Instant Feedback</h4>
                <p className="text-foreground/70">
                  Get immediate results and detailed feedback. Because we all know that being able to solve algorithm
                  puzzles is more important than being able to build real software.
                </p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">📊</div>
                <h4 className="text-xl font-semibold mb-3 text-secondary">Track Progress</h4>
                <p className="text-foreground/70">
                  Monitor your improvement and watch your impostor syndrome grow with detailed execution statistics.
                </p>
              </CardBody>
            </Card>
          </div>
        </section>

        <section className="mb-12 text-center">
          <div className="max-w-2xl mx-auto">
            <h3 className="text-2xl font-semibold mb-4 text-foreground">Don't want to sign up?</h3>
            <p className="text-lg text-foreground/70 mb-6">
              You can still browse and practice coding problems without creating an account.
            </p>
            <Button
              onPress={() => navigate('/problems')}
              variant="ghost"
              color="secondary"
              size="lg"
              className="font-medium"
            >
              Browse Problems →
            </Button>
          </div>
        </section>
      </main>

      <footer className="border-t border-content3 bg-content1">
        <div className="container mx-auto px-6 py-8 text-center">
          <p className="text-foreground/60 mb-4">&copy; 2025 DerpCode. Start your coding journey today!</p>
          <div className="flex justify-center space-x-6 text-sm">
            <button
              onClick={() => navigate('/privacy')}
              className="text-foreground/60 hover:text-primary transition-colors"
            >
              Privacy Policy
            </button>
          </div>
        </div>
      </footer>
    </div>
  );
}
