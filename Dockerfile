FROM jupyter/base-notebook:ubuntu-22.04

# Install .NET CLI dependencies

ARG NB_USER=jovyan
ARG NB_UID=1000
ENV USER ${NB_USER}
ENV NB_UID ${NB_UID}
ENV HOME /home/${NB_USER}

WORKDIR ${HOME}

USER root
RUN apt-get update
RUN apt-get install -y curl

ENV \
  # Enable detection of running in a container
  DOTNET_RUNNING_IN_CONTAINER=true \
  # Enable correct mode for dotnet watch (only mode supported in a container)
  DOTNET_USE_POLLING_FILE_WATCHER=true \
  # Skip extraction of XML docs - generally not useful within an image/container - helps performance
  NUGET_XMLDOC_MODE=skip \
  # Opt out of telemetry until after we install jupyter when building the image, this prevents caching of machine id
  DOTNET_INTERACTIVE_CLI_TELEMETRY_OPTOUT=true

# Install .NET CLI dependencies
RUN apt-get update \
  && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
  libc6 \
  libgcc1 \
  libgssapi-krb5-2 \
  libicu70 \
  libssl3 \
  libstdc++6 \
  zlib1g \
  && rm -rf /var/lib/apt/lists/*

# Install .NET Core SDK

# When updating the SDK version, the sha512 value a few lines down must also be updated.
ENV DOTNET_SDK_VERSION 7.0.100
ENV DOTNET_INTERACTIVE_VERSION 1.0.357501

RUN curl -L https://dot.net/v1/dotnet-install.sh | bash -e -s -- --install-dir /usr/share/dotnet --version $DOTNET_SDK_VERSION \
  && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

# Trigger first run experience by running arbitrary command
RUN dotnet help

# Copy notebooks
COPY ./notebooks/ ${HOME}/Notebooks/

# Add package sources
COPY ./NuGet.config ${HOME}/NuGet.config

RUN chown -R ${NB_UID} ${HOME}
USER ${USER}

#Install nteract 
RUN pip install nteract_on_jupyter

# Install lastest build of Microsoft.DotNet.Interactive
RUN dotnet tool install -g Microsoft.dotnet-interactive --version $DOTNET_INTERACTIVE_VERSION --add-source "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-experimental/nuget/v3/index.json"

ENV PATH="${PATH}:${HOME}/.dotnet/tools"
RUN echo "$PATH"


RUN echo "{\
  \"asConfiguration\": {\
    \"runtime\": {\
      \"isProcessWithUI\": true\
    }\
  }\
}\
" > ${HOME}/.dotnet/tools/.store/microsoft.dotnet-interactive/${DOTNET_INTERACTIVE_VERSION}/microsoft.dotnet-interactive/${DOTNET_INTERACTIVE_VERSION}/tools/net7.0/any/Microsoft.DotNet.Interactive.App.appsettings.json
  
# Install kernel specs
RUN dotnet interactive jupyter install

# Set root to Notebooks
WORKDIR ${HOME}/Notebooks/