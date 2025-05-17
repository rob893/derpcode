FROM node:22-slim

RUN useradd -m runner
WORKDIR /home/runner

COPY . .
RUN npm install
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
