import {BaseDto} from "./baseDto";

export class ServerSendsImageAnalysisToClient extends BaseDto<ServerSendsImageAnalysisToClient> {
  result?: AnalysisResult
}

export interface AnalysisResult {
  objects?: Object[]
  requestId?: string
  metadata?: Metadata
  modelVersion?: string
}

export interface Object {
  rectangle: Rectangle
  object: string
  confidence: number
  parent: any
}

export interface Rectangle {
  x: number
  y: number
  w: number
  h: number
}

export interface Metadata {
  height: number
  width: number
  format: string
}
