# Builds preprocess.d with a pinned D toolchain, so the migrator doesn't depend on whatever D
# compiler is installed on the host. Built and run by the migrator (see Program.cs).

FROM ubuntu:24.04 AS build

ARG LDC_VERSION=1.42.0
ARG LDC_SHA256=a7bc9c956138f558cadf9c962352f59d41c80df6eb3ae3f8039f25be14a69303

RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl xz-utils gcc libc6-dev pkg-config zlib1g-dev \
    && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL -o /tmp/ldc.tar.xz \
        "https://github.com/ldc-developers/ldc/releases/download/v${LDC_VERSION}/ldc2-${LDC_VERSION}-linux-x86_64.tar.xz" \
    && echo "${LDC_SHA256}  /tmp/ldc.tar.xz" | sha256sum -c - \
    && tar -xJf /tmp/ldc.tar.xz -C /opt \
    && rm /tmp/ldc.tar.xz

ENV PATH="/opt/ldc2-${LDC_VERSION}-linux-x86_64/bin:${PATH}"

WORKDIR /src
COPY preprocess.d .
RUN dub build --single preprocess.d --build=release --compiler=ldc2

FROM ubuntu:24.04
COPY --from=build /src/pastemyst-db-preprocess /usr/local/bin/pastemyst-db-preprocess
ENTRYPOINT ["pastemyst-db-preprocess"]
