import { Link, useNavigate } from 'react-router';
import { useAuth } from '../hooks/useAuth';
import { useEffect } from 'react';
import { Button, Card, CardBody, Navbar, NavbarBrand, NavbarContent, NavbarItem } from '@heroui/react';

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
      <Navbar
        isBordered
        classNames={{
          base: 'bg-content1/95 backdrop-blur-md',
          wrapper: 'max-w-7xl'
        }}
      >
        <NavbarBrand>
          <h1 className="text-3xl font-bold text-primary">DerpCode</h1>
        </NavbarBrand>
        <NavbarContent justify="end">
          <NavbarItem>
            <Button as={Link} to="/login" variant="ghost" color="primary">
              Sign In
            </Button>
          </NavbarItem>
          <NavbarItem>
            <Button as={Link} to="/register" color="primary" variant="solid">
              Sign Up
            </Button>
          </NavbarItem>
        </NavbarContent>
      </Navbar>

      <main className="flex-1 container mx-auto px-6 py-12">
        <section className="text-center mb-20">
          <div className="max-w-4xl mx-auto">
            <h2 className="text-5xl font-bold mb-6 bg-gradient-to-r from-primary via-secondary to-primary bg-clip-text text-transparent">
              Practice Coding with DerpCode
            </h2>
            <p className="text-xl text-foreground/80 mb-8 leading-relaxed">
              Sharpen your programming skills with our collection of coding challenges. Practice algorithms, data
              structures, and problem-solving in multiple programming languages.
            </p>
            <div className="flex gap-4 justify-center">
              <Button as={Link} to="/register" color="primary" size="lg" variant="solid">
                Get Started
              </Button>
              <Button as={Link} to="/login" variant="bordered" color="primary" size="lg">
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
                  Practice in JavaScript, TypeScript, Python, and more programming languages.
                </p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">🎯</div>
                <h4 className="text-xl font-semibold mb-3 text-secondary">Difficulty Levels</h4>
                <p className="text-foreground/70">
                  From very easy to very hard - find challenges that match your skill level.
                </p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">⚡</div>
                <h4 className="text-xl font-semibold mb-3 text-primary">Instant Feedback</h4>
                <p className="text-foreground/70">Get immediate results and detailed feedback on your solutions.</p>
              </CardBody>
            </Card>
            <Card className="bg-content2 border-content3">
              <CardBody className="text-center p-6">
                <div className="text-4xl mb-4">📊</div>
                <h4 className="text-xl font-semibold mb-3 text-secondary">Track Progress</h4>
                <p className="text-foreground/70">Monitor your improvement and see detailed execution statistics.</p>
              </CardBody>
            </Card>
          </div>
        </section>
      </main>

      <footer className="border-t border-content3 bg-content1">
        <div className="container mx-auto px-6 py-8 text-center">
          <p className="text-foreground/60">&copy; 2025 DerpCode. Start your coding journey today!</p>
        </div>
      </footer>
    </div>
  );
}
