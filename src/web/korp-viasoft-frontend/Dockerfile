FROM node:22-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 4200

# Executa o comando usando o binário local do node_modules para evitar erros de versão global
CMD ["./node_modules/.bin/ng", "serve", "--host", "0.0.0.0"]