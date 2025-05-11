import express from 'express';
import cors from 'cors';
import { problems } from './problems';
import { runWorker } from './utilities';
import { driverTemplates } from './driverTemplates';
import { Problem } from './models';

const app = express();

app.use(cors());
app.use(express.json());

function mapProblem(problem: Problem): Problem {
  return {
    ...problem,
    drivers: problem.drivers.map(driver => ({
      ...driver,
      driverCode: ''
    }))
  };
}

app.get('/problems', (req, res) => {
  res.json(problems.map(mapProblem));
});

app.get('/problems/:id', (req, res) => {
  const { id } = req.params;
  const problem = problems.find(p => p.id === id);

  if (!problem) {
    res.status(404).json({ error: 'Problem not found' });
    return;
  }

  res.json(mapProblem(problem));
});

app.post('/problems', (req, res) => {
  const problem: Problem = req.body;

  if (
    !problem.id ||
    !problem.name ||
    !problem.description ||
    !problem.difficulty ||
    !problem.expectedOutput ||
    !problem.tags ||
    !problem.input ||
    !problem.drivers
  ) {
    res.status(400).json({ error: 'Invalid problem data' });
    return;
  }
  const existingProblem = problems.find(p => p.id === problem.id);
  if (existingProblem) {
    res.status(409).json({ error: 'Problem with this ID already exists' });
    return;
  }

  problems.push(problem);
  res.status(201).json(mapProblem(problem));
});

app.get('/driverTemplates', (req, res) => {
  res.json(driverTemplates);
});

app.post('/problems/:id/submissions', async (req, res) => {
  const { id } = req.params;
  const { userCode, language } = req.body;
  const problem = problems.find(p => p.id === id);

  if (!problem) {
    res.status(404).json({ error: 'Problem not found' });
    return;
  }

  const driver = problem.drivers.find(d => d.language === language);

  if (!driver) {
    res.status(400).json({ error: 'The specified language is not supported.' });
    return;
  }

  const result = await runWorker(userCode, language, problem);

  res.status(200).json(result);
});

const PORT = process.env.PORT ?? 3000;
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT.toString()}`);
});
