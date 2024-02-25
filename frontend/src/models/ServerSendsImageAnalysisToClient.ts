import {BaseDto} from "./baseDto";

export class ServerSendsImageAnalysisToClient extends BaseDto<ServerSendsImageAnalysisToClient> {
  result?: Result
}

export interface Result {
  categories: Category[]
  requestId: string
  metadata: Metadata
  modelVersion: string
}

export interface Category {
  name: string
  score: number
}

export interface Metadata {
  height: number
  width: number
  format: string
}
