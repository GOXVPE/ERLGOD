import * as express from 'express';
import * as https from 'https';
import { Server } from 'http';
import * as fs from 'fs';
import * as os from 'os';
import { createServer } from './server';

export interface Options {
  secure?: boolean;
  port?: number;
  keyfile?: string;
  certfile?: string;
}

export class RenderStreaming {
  public static run(argv: string[]) {
    const program = require('commander');
    const readOptions = (): Options => {
      if (Array.isArray(argv)) {
        program
          .usage('[options] <apps...>')
          .option('-p, --port <n>', 'Port to start the server on', process.env.PORT || 80)
          .option('-s, --secure', 'Enable HTTPS (you need server.key and server.cert)', process.env.SECURE || false)
          .option('-k, --keyfile <path>', 'https key file (default server.key)', process.env.KEYFILE || 'server.key')
          .option('-c, --certfile <path>', 'https cert file (default server.cert)', process.env.CERTFILE || 'server.cert')
          .parse(argv);
        return {
          port: program.port,
          secure: program.secure,
          keyfile: program.keyfile,
          certfile: program.certfile,
        };
      }
    };
    const options = readOptions();
    return new RenderStreaming(options);
  }

  public app: express.Application;
  public server?: Server;
  public options: Options;

  constructor(options: Options) {
    this.options = options;
    this.app = createServer();
    if (this.options.secure) {
      this.server = https.createServer({
        key: fs.readFileSync(options.keyfile),
        cert: fs.readFileSync(options.certfile),
      },                               this.app).listen(this.options.port, () => {
        const port = this.server.address()['port'];
        const addresses = this.getIPAddress();
        for (const address of addresses) {
          console.log(`https://${address}:${port}`);
        }
      });
    } else {
      this.server = this.app.listen(this.options.port, () => {
        const port = this.server.address()['port'];
        const addresses = this.getIPAddress();
        for (const address of addresses) {
          console.log(`http://${address}:${port}`);
        }
      });
    }
  }

  getIPAddress(): string[] {
    const interfaces = os.networkInterfaces();
    const addresses: string[] = [];
    for (const k in interfaces) {
      for (const k2 in interfaces[k]) {
        const address = interfaces[k][k2];
        if (address.family === 'IPv4') {
          addresses.push(address.address);
        }
      }
    }
    return addresses;
  }
}

RenderStreaming.run(process.argv);
