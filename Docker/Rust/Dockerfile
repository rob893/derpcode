FROM rust:1.94-slim

RUN useradd -m runner
WORKDIR /home/runner

RUN mkdir src
RUN echo "fn main() {}" > src/main.rs
COPY . .
COPY base_driver.txt ./src/base_driver.rs

# Must match RUSTFLAGS used at runtime to avoid invalidating the dep cache
ENV RUSTFLAGS="-Awarnings"
RUN cargo build --release
RUN chmod +x run.sh

# Let runner reuse pre-compiled dependency artifacts for incremental builds
RUN chown -R runner:runner /home/runner

USER runner
CMD ["./run.sh"]