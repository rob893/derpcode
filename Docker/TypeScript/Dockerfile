FROM node:24-slim

RUN useradd -m runner
WORKDIR /home/runner

COPY . .
COPY base-driver.txt ./base-driver.ts
RUN npm install
RUN chmod +x run.sh

USER runner
CMD ["./run.sh"]
