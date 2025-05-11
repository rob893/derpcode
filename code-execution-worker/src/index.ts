import express from 'express';
import cors from 'cors';
import { problems } from './problems';
import { runWorker } from './utilities';

const userCodeCSharp = `
using System;

public class Solution
{
    public static int Add(int a, int b)
    {
        return a + b;
    }
}
`;

const userCodeJs = `
export function add(a, b) {
    return a + b;
}
`;

const userCodeTs = `
export function add(a: number, b: number): number {
    return a + b;
}
`;

const app = express();

app.use(cors());
app.use(express.json());

app.get('/problems', (req, res) => {
  res.json(problems);
});

app.get('/problems/:id', (req, res) => {
  const { id } = req.params;
  const problem = problems.find(p => p.id === id);

  if (!problem) {
    res.status(404).json({ error: 'Problem not found' });
    return;
  }

  res.json(problem);
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

  const result = await runWorker(userCode, driver.driverCode, problem.input, driver.image, problem.expectedOutput);

  res.status(200).json(result);
});

const PORT = process.env.PORT ?? 3000;
app.listen(PORT, () => {
  console.log(`Server running on port ${PORT.toString()}`);
});
