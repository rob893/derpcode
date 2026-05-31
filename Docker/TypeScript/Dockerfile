FROM node:24-slim

RUN useradd -m runner
WORKDIR /home/runner

COPY . .
COPY base-driver.txt ./base-driver.ts
RUN npm install
RUN chmod +x run.sh

# Let the runner user write build outputs and esbuild caches alongside the root-installed node_modules
RUN chown -R runner:runner /home/runner

USER runner
CMD ["./run.sh"]
